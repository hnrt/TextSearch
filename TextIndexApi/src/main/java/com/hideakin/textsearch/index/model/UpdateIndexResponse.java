package com.hideakin.textsearch.index.model;

public class UpdateIndexResponse {

	private String status;
	
	private String path;
	
	private int textCount;
	
	public String getStatus() {
		return status;
	}

	public void setStatus(String status) {
		this.status = status;
	}

	public String getPath() {
		return path;
	}

	public void setPath(String path) {
		this.path = path;
	}

	public int getTextCount() {
		return textCount;
	}

	public void setTextCount(int textCount) {
		this.textCount = textCount;
	}

}
