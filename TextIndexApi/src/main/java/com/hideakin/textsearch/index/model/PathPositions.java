package com.hideakin.textsearch.index.model;

import java.util.ArrayList;
import java.util.List;

public class PathPositions {

	private int fid;

	private String path;
	
	private int[] positions;
	
	public PathPositions() {
		this(-1, null, null);
	}

	public PathPositions(int fid, String path, int[] positions) {
		this.fid = fid;
		this.path = path;
		this.positions = positions;
	}

	public int getFid() {
		return fid;
	}

	public void setFid(int fid) {
		this.fid = fid;
	}

	public String getPath() {
		return path;
	}

	public void setPath(String path) {
		this.path = path;
	}

	public int[] getPositions() {
		return positions;
	}

	public void setPositions(int[] positions) {
		this.positions = positions;
	}
	
	public void addPositions(int[] positions) {
		if (this.positions != null) {
			mergePositions(this.positions, positions);
		} else {
			this.positions = positions;
		}
	}
	
	private void mergePositions(int[] positions1, int[] positions2) {
		List<Integer> dst = new ArrayList<Integer>();
		int idx1 = 0;
		int idx2 = 0;
		while (true) {
			if (idx1 < positions1.length) {
				if (idx2 < positions2.length) {
					if (positions1[idx1] < positions2[idx2]) {
						dst.add(positions1[idx1++]);
					} else if (positions1[idx1] > positions2[idx2]) {
						dst.add(positions2[idx2++]);
					} else {
						dst.add(positions1[idx1++]);
						idx2++;
					}
				} else {
					do {
						dst.add(positions1[idx1++]);
					} while (idx1 < positions1.length);
					break;
				}
			} else if (idx2 < positions2.length) {
				do {
					dst.add(positions2[idx2++]);
				} while (idx2 < positions2.length);
				break;
			} else {
				break;
			}
		}
		this.positions = new int[dst.size()];
		for (int i = 0; i < this.positions.length; i++) {
			this.positions[i] = dst.get(i);
		}	
	}

}
