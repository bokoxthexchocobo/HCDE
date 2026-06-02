#include <cstdint>
#include <cstring>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
using HCDERconSocket = SOCKET;
constexpr HCDERconSocket HCDE_RCON_INVALID_SOCKET = INVALID_SOCKET;
#else
#include <arpa/inet.h>
#include <netdb.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <unistd.h>
using HCDERconSocket = int;
constexpr HCDERconSocket HCDE_RCON_INVALID_SOCKET = -1;
#endif

namespace
{
constexpr uint32_t HCDE_RCON_MAX_FRAME = 4096;

struct FOptions
{
	std::string Host = "127.0.0.1";
	int Port = 0;
	std::string Password;
	std::string Command;
};

void PrintUsage()
{
	std::cerr
		<< "Usage: hcdercon --port <port> --password <password> [--host 127.0.0.1] <command>\n"
		<< "\n"
		<< "Examples:\n"
		<< "  hcdercon --port 10667 --password secret ping\n"
		<< "  hcdercon --port 10667 --password secret status\n";
}

FOptions ParseOptions(int argc, char** argv)
{
	FOptions options;
	std::vector<std::string> commandParts;

	for (int i = 1; i < argc; ++i)
	{
		const std::string arg = argv[i] != nullptr ? argv[i] : "";
		auto requireValue = [&](const char* name) -> std::string {
			if (i + 1 >= argc)
				throw std::runtime_error(std::string("missing value for ") + name);
			return argv[++i];
		};

		if (arg == "--host" || arg == "-h")
		{
			options.Host = requireValue(arg.c_str());
		}
		else if (arg == "--port" || arg == "-p")
		{
			options.Port = std::stoi(requireValue(arg.c_str()));
		}
		else if (arg == "--password" || arg == "--pass" || arg == "-P")
		{
			options.Password = requireValue(arg.c_str());
		}
		else if (arg == "--help" || arg == "/?")
		{
			PrintUsage();
			std::exit(0);
		}
		else
		{
			commandParts.emplace_back(arg);
		}
	}

	if (options.Port <= 0 || options.Port > 65535)
		throw std::runtime_error("port must be between 1 and 65535");
	if (options.Password.empty())
		throw std::runtime_error("password is required");
	if (commandParts.empty())
		throw std::runtime_error("command is required");

	for (size_t i = 0; i < commandParts.size(); ++i)
	{
		if (i != 0)
			options.Command += ' ';
		options.Command += commandParts[i];
	}
	return options;
}

void CloseSocket(HCDERconSocket socket)
{
	if (socket == HCDE_RCON_INVALID_SOCKET)
		return;
#ifdef _WIN32
	closesocket(socket);
#else
	close(socket);
#endif
}

void EnsureSocketRuntime()
{
#ifdef _WIN32
	WSADATA data;
	if (WSAStartup(MAKEWORD(2, 2), &data) != 0)
		throw std::runtime_error("WSAStartup failed");
#endif
}

void CleanupSocketRuntime()
{
#ifdef _WIN32
	WSACleanup();
#endif
}

void SendAll(HCDERconSocket socket, const char* data, int length)
{
	int sent = 0;
	while (sent < length)
	{
		const int rc = send(socket, data + sent, length - sent, 0);
		if (rc <= 0)
			throw std::runtime_error("socket send failed");
		sent += rc;
	}
}

void RecvAll(HCDERconSocket socket, char* data, int length)
{
	int got = 0;
	while (got < length)
	{
		const int rc = recv(socket, data + got, length - got, 0);
		if (rc <= 0)
			throw std::runtime_error("socket receive failed");
		got += rc;
	}
}

void SendFrame(HCDERconSocket socket, const std::string& text)
{
	if (text.size() > HCDE_RCON_MAX_FRAME)
		throw std::runtime_error("command frame is too large");

	const uint32_t len = static_cast<uint32_t>(text.size());
	const unsigned char header[4] =
	{
		static_cast<unsigned char>((len >> 24) & 0xff),
		static_cast<unsigned char>((len >> 16) & 0xff),
		static_cast<unsigned char>((len >> 8) & 0xff),
		static_cast<unsigned char>(len & 0xff)
	};
	SendAll(socket, reinterpret_cast<const char*>(header), 4);
	SendAll(socket, text.data(), static_cast<int>(text.size()));
}

std::string RecvFrame(HCDERconSocket socket)
{
	unsigned char header[4] = {};
	RecvAll(socket, reinterpret_cast<char*>(header), 4);
	const uint32_t len =
		(uint32_t(header[0]) << 24) |
		(uint32_t(header[1]) << 16) |
		(uint32_t(header[2]) << 8) |
		uint32_t(header[3]);
	if (len == 0 || len > HCDE_RCON_MAX_FRAME)
		throw std::runtime_error("invalid frame length from server");

	std::string text(len, '\0');
	RecvAll(socket, text.data(), static_cast<int>(len));
	return text;
}

uint32_t HCDERconHash(const std::string& text)
{
	uint32_t hash = 2166136261u;
	for (const unsigned char c : text)
	{
		hash ^= c;
		hash *= 16777619u;
	}
	return hash;
}

std::string Hex8(uint32_t value)
{
	const char* digits = "0123456789abcdef";
	std::string out(8, '0');
	for (int i = 7; i >= 0; --i)
	{
		out[size_t(i)] = digits[value & 0xf];
		value >>= 4;
	}
	return out;
}

HCDERconSocket Connect(const FOptions& options)
{
	HCDERconSocket socketHandle = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (socketHandle == HCDE_RCON_INVALID_SOCKET)
		throw std::runtime_error("socket creation failed");

	sockaddr_in addr = {};
	addr.sin_family = AF_INET;
	addr.sin_port = htons(static_cast<uint16_t>(options.Port));
	if (inet_pton(AF_INET, options.Host.c_str(), &addr.sin_addr) != 1)
	{
		CloseSocket(socketHandle);
		throw std::runtime_error("host must be an IPv4 address, usually 127.0.0.1");
	}

	if (connect(socketHandle, reinterpret_cast<sockaddr*>(&addr), sizeof(addr)) != 0)
	{
		CloseSocket(socketHandle);
		throw std::runtime_error("connect failed");
	}
	return socketHandle;
}
}

int main(int argc, char** argv)
{
	try
	{
		const FOptions options = ParseOptions(argc, argv);
		EnsureSocketRuntime();
		HCDERconSocket socketHandle = Connect(options);

		const std::string hello = RecvFrame(socketHandle);
		const std::string prefix = "nonce ";
		if (hello.rfind(prefix, 0) != 0)
			throw std::runtime_error("server did not send an RCON nonce");

		const std::string nonce = hello.substr(prefix.size());
		SendFrame(socketHandle, "auth " + Hex8(HCDERconHash(nonce + ":" + options.Password)));
		const std::string auth = RecvFrame(socketHandle);
		if (auth.rfind("OK", 0) != 0)
			throw std::runtime_error(auth);

		SendFrame(socketHandle, options.Command);
		std::cout << RecvFrame(socketHandle) << "\n";

		CloseSocket(socketHandle);
		CleanupSocketRuntime();
		return 0;
	}
	catch (const std::exception& ex)
	{
		std::cerr << "hcdercon: " << ex.what() << "\n";
		PrintUsage();
		CleanupSocketRuntime();
		return 1;
	}
}
