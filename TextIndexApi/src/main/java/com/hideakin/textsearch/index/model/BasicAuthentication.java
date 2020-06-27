package com.hideakin.textsearch.index.model;

import java.nio.charset.StandardCharsets;
import java.util.Base64;

public class BasicAuthentication {

	private static final String BASIC_SP = "Basic ";

	private String username;
	
	private String password;
	
	private BasicAuthentication(String username, String password) {
		this.username = username;
		this.password = password;
	}
	
	public String getUsername() {
		return username;
	}

	public String getPassword() {
		return password;
	}

	public static BasicAuthentication parse(String value) {
		if (value.startsWith(BASIC_SP)) {
			String s = value.substring(BASIC_SP.length()).trim();
			try {
				byte[] b = Base64.getDecoder().decode(s);
				String ucp = new String(b, StandardCharsets.UTF_8);
				String[] ss = ucp.split(":");
				if (ss.length == 2) {
					return new BasicAuthentication(ss[0], ss[1]);
				}
			} catch (IllegalArgumentException e) {
			}
		}
		return null;
	}

}
