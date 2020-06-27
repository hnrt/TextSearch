package com.hideakin.textsearch.index.model;

import java.util.HashMap;
import java.util.Map;

import org.springframework.http.HttpMethod;

public class MethodRoleCollection {

	private Map<HttpMethod,String> map;
	
	public MethodRoleCollection() {
		map = new HashMap<HttpMethod,String>();
	}
	
	public MethodRoleCollection add(HttpMethod method, String role) {
		map.put(method, role);
		return this;
	}

	public String find(String method) {
		return map.get(HttpMethod.valueOf(method));
	}
	
	public String find(HttpMethod method) {
		return map.get(method);
	}

}
