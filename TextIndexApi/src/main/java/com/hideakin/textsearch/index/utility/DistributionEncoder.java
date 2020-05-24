package com.hideakin.textsearch.index.utility;

import java.util.ArrayList;
import java.util.List;

public class DistributionEncoder {

	private List<Byte> buf;

	public DistributionEncoder() {
		buf = new ArrayList<Byte>();
	}
	
	public List<Byte> getBuf() {
		return buf;
	}

	public void write(int value) throws Exception {
		if (value < 0) {
			throw new Exception("DistributionEncoder.write: Invalid value.");
		} else if (value < 0x80) {
			byte b0 = (byte)value;
			buf.add(b0);
		} else if (value < 0x800) {
			byte b0 = (byte)(((value >> (6 * 1)) & 0x1F) | 0xC0);
			byte b1 = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			buf.add(b0);
			buf.add(b1);
		} else if (value < 0x10000) {
			byte b0 = (byte)(((value >> (6 * 2)) & 0x0F) | 0xE0);
			byte b1 = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
			byte b2 = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			buf.add(b0);
			buf.add(b1);
			buf.add(b2);
		} else if (value < 0x200000) {
			byte b0 = (byte)(((value >> (6 * 3)) & 0x07) | 0xF0);
			byte b1 = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
			byte b2 = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
			byte b3 = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			buf.add(b0);
			buf.add(b1);
			buf.add(b2);
			buf.add(b3);
		} else if (value < 0x40000000) {
			byte b0 = (byte)(((value >> (6 * 4)) & 0x03) | 0xF8);
			byte b1 = (byte)(((value >> (6 * 3)) & 0x3F) | 0x80);
			byte b2 = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
			byte b3 = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
			byte b4 = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			buf.add(b0);
			buf.add(b1);
			buf.add(b2);
			buf.add(b3);
			buf.add(b4);
		} else {
			byte b0 = (byte)(((value >> (6 * 5)) & 0x01) | 0xFC);
			byte b1 = (byte)(((value >> (6 * 4)) & 0x3F) | 0x80);
			byte b2 = (byte)(((value >> (6 * 3)) & 0x3F) | 0x80);
			byte b3 = (byte)(((value >> (6 * 2)) & 0x3F) | 0x80);
			byte b4 = (byte)(((value >> (6 * 1)) & 0x3F) | 0x80);
			byte b5 = (byte)(((value >> (6 * 0)) & 0x3F) | 0x80);
			buf.add(b0);
			buf.add(b1);
			buf.add(b2);
			buf.add(b3);
			buf.add(b4);
			buf.add(b5);
		}
	}
	
	public byte[] flush() {
		byte[] bb = new byte[buf.size()];
		for (int i = 0; i < buf.size(); i++) {
			bb[i] = buf.get(i);
		}
		return bb;
	}

}
