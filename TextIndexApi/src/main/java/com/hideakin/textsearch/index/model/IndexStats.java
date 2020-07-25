package com.hideakin.textsearch.index.model;

import com.fasterxml.jackson.annotation.JsonProperty;

public class IndexStats {

	@JsonProperty("gid")
	private int gid;
	
	@JsonProperty("group")
	private String group;

	@JsonProperty("file_count")
	private int fileCount;

	@JsonProperty("text_count")
	private int textCount;

	@JsonProperty("total_bytes")
	private long totalBytes;

	@JsonProperty("total_count")
	private long totalCount;

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

	public int getFileCount() {
		return fileCount;
	}

	public void setFileCount(int fileCount) {
		this.fileCount = fileCount;
	}

	public int getTextCount() {
		return textCount;
	}

	public void setTextCount(int textCount) {
		this.textCount = textCount;
	}

	public long getTotalBytes() {
		return totalBytes;
	}

	public void setTotalBytes(long totalBytes) {
		this.totalBytes = totalBytes;
	}

	public long getTotalCount() {
		return totalCount;
	}

	public void setTotalCount(long totalCount) {
		this.totalCount = totalCount;
	}

}
