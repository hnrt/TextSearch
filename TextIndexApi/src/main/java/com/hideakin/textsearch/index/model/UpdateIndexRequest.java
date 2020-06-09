package com.hideakin.textsearch.index.model;

public class UpdateIndexRequest {

	private String path;
	
	private String[] texts;
	
	public UpdateIndexRequest() {
		this.path = null;
		this.texts = null;
	}

	public UpdateIndexRequest(String path, String[] texts) {
		this.path = path;
		this.texts = texts;
	}

	public String getPath() {
		return path;
	}

	public void setPath(String path) {
		this.path = path;
	}

	public String[] getTexts() {
		return texts;
	}

	public void setTexts(String[] texts) {
		this.texts = texts;
	}

}
