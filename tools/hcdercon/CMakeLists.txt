cmake_minimum_required( VERSION 3.16 )

add_executable( hcdercon hcdercon.cpp )
target_compile_features( hcdercon PRIVATE cxx_std_17 )
set_target_properties( hcdercon PROPERTIES RUNTIME_OUTPUT_DIRECTORY ${HCDE_OUTPUT_DIR} )

if( WIN32 )
	target_link_libraries( hcdercon ws2_32 )
endif()

install( TARGETS hcdercon RUNTIME DESTINATION "." )
