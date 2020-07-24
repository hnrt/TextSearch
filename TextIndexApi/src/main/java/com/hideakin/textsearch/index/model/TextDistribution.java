package com.hideakin.textsearch.index.model;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;

public class TextDistribution {

	private int fid;
	
	private int[] positions;

	public TextDistribution() {
		fid = -1;
		positions = null;
	}

	public TextDistribution(int fid, int[] positions) {
		this.fid = fid;
		this.positions = positions;
	}

	public TextDistribution(int fid, List<Integer> positions) {
		this.fid = fid;
		this.positions = new int[positions.size()];
		for (int i = 0; i < this.positions.length; i++) {
			this.positions[i] = positions.get(i);
		}
	}

	public int getFid() {
		return fid;
	}

	public int[] getPositions() {
		return positions;
	}
	
	public void addPositions(int[] src) {
		positions = mergePositions(positions, src);
	}

	private static int[] mergePositions(int[] src1, int[] src2) {
		if (src1 == null || src1.length == 0) {
			return src2;
		} else if (src2 == null || src2.length == 0) {
			return src1;
		}
		int[] dst = new int[src1.length + src2.length];
		int val1 = src1[0];
		int val2 = src2[0];
		int idx1 = 1;
		int idx2 = 1;
		int idx3 = 0;
		while (true) {
			if (val1 < val2) {
				dst[idx3++] = val1;
				if (idx1 < src1.length) {
					val1 = src1[idx1++];
				} else {
					dst[idx3++] = val2;
					if (idx2 < src2.length) {
						System.arraycopy(src2, idx2, dst, idx3, src2.length - idx2);
					}
					break;
				}
			} else if (val1 > val2) {
				dst[idx3++] = val2;
				if (idx2 < src2.length) {
					val2 = src2[idx2++];
				} else {
					dst[idx3++] = val1;
					if (idx1 < src1.length) {
						System.arraycopy(src1, idx1, dst, idx3, src1.length - idx1);
					}
					break;
				}
			} else {
				throw new RuntimeException("TextDistribution.mergePositions: Duplicate positions.");
			}
		}
		return dst;
	}

	public Packed pack() {
		return new Packed(this.fid, this.positions);
	}
	
	public static Packed pack(int fid, int[] positions) {
		return new Packed(fid, positions);
	}
	
	public static Packed pack(int fid, List<Integer> positions) {
		return new Packed(fid, positions);
	}
	
	public static PackedSequence sequence(byte[] data) {
		return new PackedSequence(data);
	}

	public static PackedSequence sequence(Packed[] data) {
		return (new PackedSequence(new byte[0])).append(data);
	}

	public static class Packed {
		
		private byte[] buf;
		
		private int len;
		
		private Packed(int fid, int[] positions) {
			if (positions != null && positions.length > 0) {
				buf = new byte[(2 + positions.length) * 6];
				len = 0;
				write(fid);
				write(positions.length);
				for (int position : positions) {
					write(position);
				}
			} else {
				buf = new byte[0];
				len = 0;
			}
		}
		
		private Packed(int fid, List<Integer> positions) {
			if (positions != null && positions.size() > 0) {
				buf = new byte[(2 + positions.size()) * 6];
				len = 0;
				write(fid);
				write(positions.size());
				for (int position : positions) {
					write(position);
				}
			} else {
				buf = new byte[0];
				len = 0;
			}
		}
		
		public byte[] array() {
			return buf;
		}
		
		public int length() {
			return len;
		}
		
		public byte[] copy() {
			byte[] data = new byte[len];
			System.arraycopy(buf, 0, data, 0, len);
			return data;
		}
		
		private void write(int value) {
			if (value < 0) {
				throw new RuntimeException("TextDistribution.Packed.write: Invalid value.");
			} else if (value < 0x80) {
				buf[len++] = (byte)value;
			} else if (value < 0x800) {
				buf[len++] = (byte)(((value >> (6 * 1)) & 0x1F) | 0xC0);
				buf[len++] = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			} else if (value < 0x10000) {
				buf[len++] = (byte)(((value >> (6 * 2)) & 0x0F) | 0xE0);
				buf[len++] = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			} else if (value < 0x200000) {
				buf[len++] = (byte)(((value >> (6 * 3)) & 0x07) | 0xF0);
				buf[len++] = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			} else if (value < 0x4000000) {
				buf[len++] = (byte)(((value >> (6 * 4)) & 0x03) | 0xF8);
				buf[len++] = (byte)(((value >> (6 * 3)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			} else {
				buf[len++] = (byte)(((value >> (6 * 5)) & 0x01) | 0xFC);
				buf[len++] = (byte)(((value >> (6 * 4)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 3)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
				buf[len++] = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			}
		}

	}
	
	public static class PackedSequence {
		
		private byte[] src;
		
		private int index;

		private PackedSequence(byte[] src) {
			this.src = src;
			this.index = 0;
		}

		public byte[] array() {
			return src;
		}
		
		public void rewind() {
			index = 0;
		}
		
		public TextDistribution get() {
			if (index >= src.length) {
				return null;
			}
			int fid = read();
			if (fid < 0) {
				throw new RuntimeException("TextDistribution.PackedSequence: Invalid FID value.");
			}
			int count = read();
			if (count < 0) {
				throw new RuntimeException("TextDistribution.PackedSequence: Invalid count value.");
			}
			List<Integer> positions = new ArrayList<>();
			int previous = -1;
			for (int i = 0; i < count; i++) {
				int position = read();
				if (position <= previous) {
					throw new RuntimeException("TextDistribution.PackedSequence: Invalid position value.");
				}
				positions.add(position);
				previous = position;
			}
			return new TextDistribution(fid, positions);
		}
		
		public PackedSequence append(Packed packed) {
			if (packed.length() > 0) {
				byte[] next;
				if (src != null && src.length > 0) {
					next = new byte[src.length + packed.length()];
					System.arraycopy(src, 0, next, 0, src.length);
					System.arraycopy(packed.array(), 0, next, src.length, packed.length());
				} else {
					next = new byte[packed.length()];
					System.arraycopy(packed.array(), 0, next, 0, packed.length());
				}
				src = next;
			}
			return this;
		}
		
		public PackedSequence append(Packed[] packedArray) {
			int length = src != null ? src.length : 0;
			for (Packed packed : packedArray) {
				length += packed.length();
			}
			byte[] next = new byte[length];
			if (src != null) {
				System.arraycopy(src, 0, next, 0, src.length);
				length = src.length;
			} else {
				length = 0;
			}
			for (Packed packed : packedArray) {
				System.arraycopy(packed.array(), 0, next, length, packed.length());
				length += packed.length();
			}
			src = next;
			return this;
		}
		
		public PackedSequence remove(Set<Integer> fids) {
			List<Range> ranges = new ArrayList<>();
			int size = 0;
			int startIndex = -1;
			int endIndex;
			index = 0;
			while ((endIndex = index) < src.length) {
				int currentFid = read();
				if (currentFid < 0) {
					throw new RuntimeException("TextDistribution.PackedSequence: Invalid FID value.");
				}
				int count = read();
				if (count < 0) {
					throw new RuntimeException("TextDistribution.PackedSequence: Invalid count value.");
				}
				int previous = -1;
				for (int i = 0; i < count; i++) {
					int position = read();
					if (position <= previous) {
						throw new RuntimeException("TextDistribution.PackedSequence: Invalid position value.");
					}
					previous = position;
				}
				if (fids.contains(currentFid)) {
					if (startIndex >= 0) {
						Range range = new Range(startIndex, endIndex);
						ranges.add(range);
						size += range.length();
						startIndex = -1;
					}
				} else if (startIndex < 0) {
					startIndex = endIndex;
				}
			}
			if (startIndex >= 0) {
				Range range = new Range(startIndex, endIndex);
				ranges.add(range);
				size += range.length();
			}
			if (size < src.length) {
				if (size > 0) {
					byte[] next = new byte[size];
					index = 0;
					for (Range range : ranges) {
						System.arraycopy(src, range.index(), next, index, range.length());
						index += range.length();
					}
					src = next;
				} else {
					src = new byte[0];
				}
			}
			return this;
		}

		private int read() {
			if (index < src.length) {
				byte b0 = src[index++];
				if ((b0 & 0x80) == 0) {
					return (int)b0;
				} else if ((b0 & 0xE0) == 0xC0) {
					if (index + 1 <= src.length) {
						byte b1 = src[index++];
						return (((int)(b0 & 0x1F)) << (6 * 1))
						     | (((int)(b1 & 0x3F)) << (6 * 0));
					}
				} else if ((b0 & 0xF0) == 0xE0) {
					if (index + 2 <= src.length) {
						byte b1 = src[index++];
						byte b2 = src[index++];
						return (((int)(b0 & 0x0F)) << (6 * 2))
						     | (((int)(b1 & 0x3F)) << (6 * 1))
						     | (((int)(b2 & 0x3F)) << (6 * 0));
					}
				} else if ((b0 & 0xF8) == 0xF0) {
					if (index + 3 <= src.length) {
						byte b1 = src[index++];
						byte b2 = src[index++];
						byte b3 = src[index++];
						return (((int)(b0 & 0x07)) << (6 * 3))
						     | (((int)(b1 & 0x3F)) << (6 * 2))
						     | (((int)(b2 & 0x3F)) << (6 * 1))
						     | (((int)(b3 & 0x3F)) << (6 * 0));
					}
				} else if ((b0 & 0xFC) == 0xF8) {
					if (index + 4 <= src.length) {
						byte b1 = src[index++];
						byte b2 = src[index++];
						byte b3 = src[index++];
						byte b4 = src[index++];
						return (((int)(b0 & 0x03)) << (6 * 4))
						     | (((int)(b1 & 0x3F)) << (6 * 3))
						     | (((int)(b2 & 0x3F)) << (6 * 2))
						     | (((int)(b3 & 0x3F)) << (6 * 1))
						     | (((int)(b4 & 0x3F)) << (6 * 0));
					}
				} else if ((b0 & 0xFE) == 0xFC) {
					if (index + 5 <= src.length) {
						byte b1 = src[index++];
						byte b2 = src[index++];
						byte b3 = src[index++];
						byte b4 = src[index++];
						byte b5 = src[index++];
						return (((int)(b0 & 0x01)) << (6 * 5))
						     | (((int)(b1 & 0x3F)) << (6 * 4))
						     | (((int)(b2 & 0x3F)) << (6 * 3))
						     | (((int)(b3 & 0x3F)) << (6 * 2))
						     | (((int)(b4 & 0x3F)) << (6 * 1))
						     | (((int)(b5 & 0x3F)) << (6 * 0));
					}
				} else {
					throw new RuntimeException("TextDistribution.PackedSequence: Invalid data.");
				}
			}
			throw new RuntimeException("TextDistribution.PackedSequence: Unexpected end of data.");
		}

	}
	
	private static class Range {

		private int start;

		private int end;

		public Range(int start, int end) {
			this.start = start;
			this.end = end;
		}

		public int index() {
			return start;
		}

		public int length() {
			return end - start;
		}

	}

}
