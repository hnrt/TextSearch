package com.hideakin.textsearch.index.model;

public class BearerToken {

	private static final String BEARER_SP = "Bearer ";

	private String token;
	
	private BearerToken(String token) {
		this.token = token;
	}

	public String getToken() {
		return token;
	}

	public static BearerToken parse(String value) {
		if (value.startsWith(BEARER_SP)) {
			return new BearerToken(value.substring(BEARER_SP.length()).trim());
		}
		return null;
	}

}
