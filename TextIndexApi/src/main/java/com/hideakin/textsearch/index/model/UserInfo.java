package com.hideakin.textsearch.index.model;

import java.time.ZonedDateTime;

public class UserInfo {

	private int uid;
	
	private String username;
	
	private String roles;
	
	private ZonedDateTime createdAt;
	
	private ZonedDateTime updatedAt;
	
	private ZonedDateTime expiry;
	
	private String apiKey;

	public int getUid() {
		return uid;
	}

	public void setUid(int uid) {
		this.uid = uid;
	}

	public String getUsername() {
		return username;
	}

	public void setUsername(String username) {
		this.username = username;
	}

	public String getRoles() {
		return roles;
	}

	public void setRoles(String roles) {
		this.roles = roles;
	}

	public ZonedDateTime getCreatedAt() {
		return createdAt;
	}

	public void setCreatedAt(ZonedDateTime createdAt) {
		this.createdAt = createdAt;
	}

	public ZonedDateTime getUpdatedAt() {
		return updatedAt;
	}

	public void setUpdatedAt(ZonedDateTime updatedAt) {
		this.updatedAt = updatedAt;
	}

	public ZonedDateTime getExpiry() {
		return expiry;
	}

	public void setExpiry(ZonedDateTime expiry) {
		this.expiry = expiry;
	}

	public String getApiKey() {
		return apiKey;
	}

	public void setApiKey(String apiKey) {
		this.apiKey = apiKey;
	}

}
