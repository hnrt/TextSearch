package com.hideakin.textsearch.index.model;

import java.time.ZonedDateTime;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.hideakin.textsearch.index.entity.FileGroupEntity;

public class FileGroupInfo {

	@JsonProperty("gid")
	private int gid;
	
	@JsonProperty("name")
	private String name;
	
	@JsonProperty("created_at")
	private ZonedDateTime createdAt;
	
	@JsonProperty("updated_at")
	private ZonedDateTime updatedAt;

	public FileGroupInfo() {
		this(-1, null, null, null);
	}

	public FileGroupInfo(FileGroupEntity entity) {
		this(entity.getGid(), entity.getName(), entity.getCreatedAt(), entity.getUpdatedAt());
	}

	public FileGroupInfo(int gid, String name) {
		this(gid, name, ZonedDateTime.now(), ZonedDateTime.now());
	}

	public FileGroupInfo(int gid, String name, ZonedDateTime createdAt, ZonedDateTime updatedAt) {
		this.gid = gid;
		this.name = name;
		this.createdAt = createdAt;
		this.updatedAt = updatedAt;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
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

}
