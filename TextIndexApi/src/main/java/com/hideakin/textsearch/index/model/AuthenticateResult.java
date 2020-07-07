package com.hideakin.textsearch.index.model;

public class AuthenticateResult {

	private String accessToken;
	
	private int expiresIn;
	
	public AuthenticateResult(String accessToken, int expiresIn) {
		this.accessToken = accessToken;
		this.expiresIn = expiresIn;
	}

	public String getAccessToken() {
		return accessToken;
	}

	public int getExpiresIn() {
		return expiresIn;
	}

}
