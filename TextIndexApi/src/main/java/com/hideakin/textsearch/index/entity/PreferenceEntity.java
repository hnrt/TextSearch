package com.hideakin.textsearch.index.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

@Entity(name = "preferences")
@Table(name = "preferences")
public class PreferenceEntity {

	@Id
	@Column(name="name")
	private String name;

	@Column(name="value")
	private String value;

	public PreferenceEntity() {
		this.name = null;
		this.value = null;
	}
	
	public PreferenceEntity(String name) {
		this.name = name;
		this.value = null;
	}
	
	public PreferenceEntity(String name, String value) {
		this.name = name;
		this.value = value;
	}
	
	public PreferenceEntity(String name, int value) {
		this.name = name;
		this.value = String.format("%d", value);
	}
	
	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public String getValue() {
		return value;
	}

	public void setValue(String value) {
		this.value = value;
	}

	public int getIntValue() {
		return Integer.parseInt(value, 10);
	}

	public void setValue(int value) {
		this.value = String.format("%d", value);
	}

}
