package com.hideakin.textsearch.index.model;

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;

public class FileInfo {

	private int fid;

	private String path;

	private int size;

	private int gid;

	private String group;

	public FileInfo(int fid, String path, int size, int gid, String group) {
		this.fid = fid;
		this.path = path;
		this.size = size;
		this.gid = gid;
		this.group = group;
	}

	public FileInfo(FileEntity file, FileGroupEntity group) {
		this(file.getFid(),
			file.getPath(),
			file.getSize(),
			group.getGid(),
			group.getName());
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

	public int getSize() {
		return size;
	}

	public void setSize(int size) {
		this.size = size;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

	public String getGroup() {
		return group;
	}

	public void setGroup(String group) {
		this.group = group;
	}

}
