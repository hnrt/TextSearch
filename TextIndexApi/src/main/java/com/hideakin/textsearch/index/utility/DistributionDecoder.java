package com.hideakin.textsearch.index.utility;

import com.hideakin.textsearch.index.model.Distribution;

public class DistributionDecoder {

	private byte[] data;
	private int nextIndex;
	private int startIndex;

	public DistributionDecoder(byte[] data) {
		this.data = data;
		this.nextIndex = 0;
	}

	public Distribution get() {
		int fid = read();
		if (fid >= 0) {
			int count = read();
			if (count < 0) {
				nextIndex = data.length;
				return null;
			}
			int[] positions = new int[count];
			for (int i = 0; i < count; i++) {
				int position = read();
				if (position < 0) {
					nextIndex = data.length;
					return null;
				}
				positions[i] = position;
			}
			Distribution dist = new Distribution(fid, positions);
			return dist;			
		} else {
			nextIndex = data.length;
			return null;
		}
	}
	
	public int read() {
		startIndex = nextIndex;
		if (nextIndex + 1 <= data.length) {
			byte b0 = data[nextIndex++];
			if (b0 < 0x80) {
				return (int)b0;
			}
			else if ((b0 & 0xE0) == 0xC0) {
				if (nextIndex + 1 <= data.length) {
					byte b1 = data[nextIndex++];
					return (((int)(b0 & 0x1F)) << (6 * 1))
					     | (((int)(b1 & 0x3F)) << (6 * 0));
				}
			}
			else if ((b0 & 0xF0) == 0xE0) {
				if (nextIndex + 2 <= data.length) {
					byte b1 = data[nextIndex++];
					byte b2 = data[nextIndex++];
					return (((int)(b0 & 0x0F)) << (6 * 2))
					     | (((int)(b1 & 0x3F)) << (6 * 1))
					     | (((int)(b2 & 0x3F)) << (6 * 0));
				}
			}
			else if ((b0 & 0xF8) == 0xF0) {
				if (nextIndex + 3 <= data.length) {
					byte b1 = data[nextIndex++];
					byte b2 = data[nextIndex++];
					byte b3 = data[nextIndex++];
					return (((int)(b0 & 0x07)) << (6 * 3))
					     | (((int)(b1 & 0x3F)) << (6 * 2))
					     | (((int)(b2 & 0x3F)) << (6 * 1))
					     | (((int)(b3 & 0x3F)) << (6 * 0));
				}
			}
			else if ((b0 & 0xFC) == 0xF8) {
				if (nextIndex + 4 <= data.length) {
					byte b1 = data[nextIndex++];
					byte b2 = data[nextIndex++];
					byte b3 = data[nextIndex++];
					byte b4 = data[nextIndex++];
					return (((int)(b0 & 0x03)) << (6 * 4))
					     | (((int)(b1 & 0x3F)) << (6 * 3))
					     | (((int)(b2 & 0x3F)) << (6 * 2))
					     | (((int)(b3 & 0x3F)) << (6 * 1))
					     | (((int)(b4 & 0x3F)) << (6 * 0));
				}
			}
			else if ((b0 & 0xFE) == 0xFC) {
				if (nextIndex + 5 <= data.length) {
					byte b1 = data[nextIndex++];
					byte b2 = data[nextIndex++];
					byte b3 = data[nextIndex++];
					byte b4 = data[nextIndex++];
					byte b5 = data[nextIndex++];
					return (((int)(b0 & 0x01)) << (6 * 5))
					     | (((int)(b1 & 0x3F)) << (6 * 4))
					     | (((int)(b2 & 0x3F)) << (6 * 3))
					     | (((int)(b3 & 0x3F)) << (6 * 2))
					     | (((int)(b4 & 0x3F)) << (6 * 1))
					     | (((int)(b5 & 0x3F)) << (6 * 0));
				}
			}
		}
		return -1;
	}
	
	public int getIndex() {
		return startIndex;
	}
	
	public int getLength() {
		return nextIndex - startIndex;
	}

}
