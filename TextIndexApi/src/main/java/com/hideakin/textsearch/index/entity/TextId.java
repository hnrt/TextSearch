package com.hideakin.textsearch.index.entity;

import java.io.Serializable;

import javax.persistence.Column;

public class TextId implements Serializable {

	private static final long serialVersionUID = 5334476672473568911L;

	@Column(name="txt")
	private String text;
	
	@Column(name="gid")
	private int gid;

	public TextId() {
		this(null, -1);
	}

	public TextId(String text, int gid) {
		this.text = text;
		this.gid = gid;
	}

	public String getText() {
		return text;
	}

	public void setText(String text) {
		this.text = text;
	}

	public int getGid() {
		return gid;
	}

	public void setGid(int gid) {
		this.gid = gid;
	}

}
