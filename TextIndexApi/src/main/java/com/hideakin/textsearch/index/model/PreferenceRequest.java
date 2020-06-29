package com.hideakin.textsearch.index.model;

public class PreferenceRequest {

	private String name;
	
	private String value;
	
	public PreferenceRequest() {
	}

	public PreferenceRequest(String name, String value) {
		this.name = name;
		this.value = value;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public String getValue() {
		return value;
	}

	public void setValue(String value) {
		this.value = value;
	}

}
