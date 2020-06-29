package com.hideakin.textsearch.index.model;

import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;

import org.springframework.http.HttpMethod;

import com.hideakin.textsearch.index.utility.RoleComparator;

public class MethodRoleCollection {
	
	private Map<HttpMethod,String[]> map;
	
	private RoleComparator cmp = new RoleComparator();

	public MethodRoleCollection() {
		map = new HashMap<HttpMethod,String[]>();
	}
	
	public MethodRoleCollection add(HttpMethod method, String role) {
		map.put(method, new String[] { role });
		return this;
	}

	public MethodRoleCollection add(HttpMethod method, String role1, String role2) {
		String[] roles = new String[] { role1, role2 };
		Arrays.sort(roles, cmp);
		map.put(method, roles);
		return this;
	}

	public MethodRoleCollection add(HttpMethod method, String role1, String role2, String role3) {
		String[] roles = new String[] { role1, role2, role3 };
		Arrays.sort(roles, cmp);
		map.put(method, roles);
		return this;
	}

	public String[] find(String method) {
		return map.get(HttpMethod.valueOf(method));
	}
	
	public String[] find(HttpMethod method) {
		return map.get(method);
	}

}
