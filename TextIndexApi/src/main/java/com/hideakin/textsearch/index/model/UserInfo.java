package com.hideakin.textsearch.index.model;

import java.time.ZonedDateTime;

import com.hideakin.textsearch.index.entity.UserEntity;

public class UserInfo {

	private int uid;
	
	private String username;
	
	private String[] roles;
	
	private ZonedDateTime createdAt;
	
	private ZonedDateTime updatedAt;
	
	private String accessToken;
	
	private ZonedDateTime expiresAt;
	
	public UserInfo(UserEntity entity) {
		this.uid = entity.getUid();
		this.username = entity.getUsername();
		this.setRoles(entity.getRoles());
		this.createdAt = entity.getCreatedAt();
		this.updatedAt = entity.getUpdatedAt();
		this.accessToken = entity.getAccessToken();
		this.expiresAt = entity.getExpiresAt();
	}

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

	public String[] getRoles() {
		return roles;
	}

	public void setRoles(String[] roles) {
		this.roles = roles;
	}

	public void setRoles(String roles) {
		this.roles = roles != null ? roles.split(",") : new String[0];
	}
	
	public boolean isAdministrator() {
		for (String role : roles) {
			if (role.equalsIgnoreCase("administrator")) {
				return true;
			}
		}
		return false;
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

	public String getAccessToken() {
		return accessToken;
	}

	public void setAccessToken(String accessToken) {
		this.accessToken = accessToken;
	}

	public ZonedDateTime getExpiresAt() {
		return expiresAt;
	}

	public void setExpiresAt(ZonedDateTime expiresAt) {
		this.expiresAt = expiresAt;
	}

}
