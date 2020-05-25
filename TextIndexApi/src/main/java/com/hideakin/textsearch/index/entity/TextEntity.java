package com.hideakin.textsearch.index.entity;

import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

import com.hideakin.textsearch.index.model.Distribution;
import com.hideakin.textsearch.index.utility.DistributionDecoder;
import com.hideakin.textsearch.index.utility.DistributionEncoder;

@Entity(name = "texts")
@Table(name = "texts")
public class TextEntity {

	@Id
	@Column(name="txt")
	private String text;
	
	@Column(name="dst")
	private byte[] dist;

	public String getText() {
		return text;
	}

	public void setText(String text) {
		this.text = text;
	}

	public byte[] getDist() {
		return dist;
	}

	public void setDist(byte[] dist) {
		this.dist = dist;
	}
	
	public void appendDist(int fid, List<Integer> positions) {
		DistributionEncoder enc = new DistributionEncoder();
		try {
			enc.write(fid);
			enc.write(positions.size());
			for (int position : positions) {
				enc.write(position);
			}
			appendDist(enc.getBuf());
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void appendDist(Distribution d) {
		DistributionEncoder enc = new DistributionEncoder();
		try {
			enc.write(d.getFid());
			enc.write(d.getPositions().length);
			for (int position : d.getPositions()) {
				enc.write(position);
			}
			appendDist(enc.getBuf());
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private void appendDist(List<Byte> byteList) {
		byte[] next;
		int dstIdx;
		if (dist != null) {
			next = new byte[dist.length + byteList.size()];
			System.arraycopy(dist, 0, next, 0, dist.length);
			dstIdx = dist.length;
		} else {
			next = new byte[byteList.size()];
			dstIdx = 0;
		}
		int srcIdx = 0;
		while (srcIdx < byteList.size()) {
			next[dstIdx++] = byteList.get(srcIdx++);
		}
		dist = next;
	}
	
	public boolean removeDistByFid(int fid) {
		DistributionDecoder dec = new DistributionDecoder(dist);
		while (true) {
			int currentFid = dec.read();
			if (currentFid < 0) {
				return false;
			}
			int startIndex = dec.getIndex();
			int count = dec.read();
			if (count < 0) {
				//ERROR
				return false;
			}
			int previous = -1;
			for (int i = 0; i < count; i++) {
				int position = dec.read();
				if (position <= previous) {
					//ERROR
					return false;
				}
			}
			if (currentFid == fid) {
				removeDist(startIndex, dec.getIndex() + dec.getLength());
				return true;
			}
		}
	}

	private void removeDist(int startIndex, int endIndex) {
		byte[] next = new byte[dist.length - (endIndex - startIndex)];
		System.arraycopy(dist, 0, next, 0, startIndex);
		System.arraycopy(dist, endIndex, next, startIndex, dist.length - endIndex);
		dist = next;
	}

}
