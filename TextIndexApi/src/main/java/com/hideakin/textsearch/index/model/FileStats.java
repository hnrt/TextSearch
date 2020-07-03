package com.hideakin.textsearch.index.model;

public class FileStats {

	private int gid;

	private String group;

	private long files;

	private long totalBytes;

	private long totalStoredBytes;

	private long staleFiles;

	private long totalStaleBytes;

	private long totalStoredStaleBytes;

	public FileStats(int gid, String group) {
		this.gid = gid;
		this.group = group;
		this.files = 0;
		this.totalBytes = 0;
		this.totalStoredBytes = 0;
		this.staleFiles = 0;
		this.totalStaleBytes = 0;
		this.totalStoredStaleBytes = 0;
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

	public long getFiles() {
		return files;
	}

	public void incFiles() {
		this.files++;
	}

	public long getTotalBytes() {
		return totalBytes;
	}

	public void addBytes(long value) {
		this.totalBytes += value;
	}

	public long getTotalStoredBytes() {
		return totalStoredBytes;
	}

	public void addStoredBytes(long value) {
		this.totalStoredBytes += value;
	}

	public long getStaleFiles() {
		return staleFiles;
	}

	public void incStaleFiles() {
		this.staleFiles++;
	}

	public long getTotalStaleBytes() {
		return totalStaleBytes;
	}

	public void addStaleBytes(long value) {
		this.totalStaleBytes += value;
	}

	public long getTotalStoredStaleBytes() {
		return totalStoredStaleBytes;
	}

	public void addStoredStaleBytes(long value) {
		this.totalStoredStaleBytes += value;
	}

}
