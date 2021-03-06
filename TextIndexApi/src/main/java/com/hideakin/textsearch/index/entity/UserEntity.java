package com.hideakin.textsearch.index.entity;

import java.time.ZonedDateTime;
import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "users")
@Table(name = "users")
public class UserEntity {
	
	@Id
	@Column(name="uid")
	private int uid;

	@Column(name="username")
	private String username;

	@Column(name="password")
	private String password;

	@Column(name="roles")
	private String roles;

	@Column(name="created_at")
	private ZonedDateTime createdAt;

	@Column(name="updated_at")
	private ZonedDateTime updatedAt;

	@Column(name="access_token")
	private String accessToken;

	@Column(name="expires_at")
	private ZonedDateTime expiresAt;

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

	public String getPassword() {
		return password;
	}

	public void setPassword(String password) {
		this.password = password;
	}

	public String getRoles() {
		return roles;
	}

	public void setRoles(String roles) {
		this.roles = roles;
	}

	public void setRoles(String[] roles) {
		StringBuilder buf = new StringBuilder();
		for (String role : roles) {
			buf.append(',');
			buf.append(role);
		}
		this.roles = buf.length() > 0 ? buf.substring(1) : null;
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
