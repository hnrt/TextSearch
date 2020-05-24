package com.hideakin.textsearch.index.model;

public class UpdateIndexRequest {

	private String path;
	
	private String[] texts;

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
