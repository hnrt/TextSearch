package com.hideakin.textsearch.index.model;

public class IdStatus {

	private int uid;

	private int gid;
	
	private int fid;
	
	public IdStatus() {
		this(-1, -1, -1);
	}

	public IdStatus(int uid, int gid, int fid) {
		this.uid = uid;
		this.gid = gid;
		this.fid = fid;
	}

	public int getUid() {
		return uid;
	}

	public void setUid(int uid) {
		this.uid = uid;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

	public int getFid() {
		return fid;
	}

	public void setFid(int fid) {
		this.fid = fid;
	}

}
