package com.hideakin.textsearch.index.model;

public class FileGroupRequest {

	private String name;

	public FileGroupRequest() {
		this(null);
	}

	public FileGroupRequest(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

}
