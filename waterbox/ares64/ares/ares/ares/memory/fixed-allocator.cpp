#include <ares/ares.hpp>

namespace ares::Memory {

#if defined(PLATFORM_MACOS)
//stub for unsupported platforms
FixedAllocator::FixedAllocator() {
}
#else
alignas(4096) u8 fixedBuffer[128_MiB] ECL_INVISIBLE;

FixedAllocator::FixedAllocator() {
  _allocator.resize(sizeof(fixedBuffer), 0, fixedBuffer);
}
#endif

auto FixedAllocator::get() -> bump_allocator& {
  static FixedAllocator allocator;
  return allocator._allocator;
}

}
