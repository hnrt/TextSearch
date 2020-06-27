package com.hideakin.textsearch.index.model;

public class AuthenticateResult {

	private String apiKey;
	
	private int expiresIn;
	
	public AuthenticateResult(String apiKey, int expiresIn) {
		this.apiKey = apiKey;
		this.expiresIn = expiresIn;
	}

	public String getApiKey() {
		return apiKey;
	}

	public int getExpiresIn() {
		return expiresIn;
	}

}
