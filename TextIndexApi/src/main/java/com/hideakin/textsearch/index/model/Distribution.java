package com.hideakin.textsearch.index.model;

import java.util.List;

public class Distribution {

	private int fid;
	
	private int[] positions;
	
	public Distribution(int fid, int[] positions) {
		this.fid = fid;
		this.positions = positions;
	}

	public Distribution(int fid, List<Integer> positions) {
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

}
