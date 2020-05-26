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

}
