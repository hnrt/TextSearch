package com.hideakin.textsearch.index.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "file_contents")
@Table(name = "file_contents")
public class FileContentEntity {

	@Id
	@Column(name="fid")
	private int fid;
	
	@Column(name="data")
	private byte[] data;

	public FileContentEntity() {
		this(-1, null);
	}

	public FileContentEntity(int fid, byte[] data) {
		this.fid = fid;
		this.data = data;
	}

	public int getFid() {
		return fid;
	}

	public void setFid(int fid) {
		this.fid = fid;
	}

	public byte[] getData() {
		return data;
	}

	public void setData(byte[] data) {
		this.data = data;
	}
	
}
