cmake_minimum_required(VERSION 2.4.3)
#set(CMAKE_ALLOW_LOOSE_LOOP_CONSTRUCTS true)

project(pngcrush C)
#enable_testing()

# Dependencies
find_package(ZLIB REQUIRED)
find_package(PNG REQUIRED)

include_directories(${ZLIB_INCLUDE_DIR} ${PNG_INCLUDE_DIR})

# OUR SOURCES
FILE(GLOB source_files *.c *.h)

set(libpng_sources
  png.h
  pngconf.h
  png.c
  pngerror.c
  pngget.c
  pngmem.c
  pngpread.c
  pngread.c
  pngrio.c
  pngrtran.c
  pngrutil.c
  pngset.c
  pngtrans.c
  pngwio.c
  pngwrite.c
  pngwtran.c
  pngwutil.c
)

add_executable(pngcrush ${source_files})
target_link_libraries(pngcrush ${PNG_LIB_NAME} ${ZLIB_LIB_NAME})

# Installation
install(TARGETS pngcrush RUNTIME DESTINATION bin)
