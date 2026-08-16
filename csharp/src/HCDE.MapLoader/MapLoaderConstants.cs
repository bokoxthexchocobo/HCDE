namespace HCDE.MapLoader;

public enum MapDataFormat : byte
{
  Unknown = 0,
  DoomBinary = 1,
  UdmfText = 2,
}

public static class MapLoaderConstants
{
  public const int MaxMapNameLength = 8;
  public const string DefaultMapName = "MAP01";
}
