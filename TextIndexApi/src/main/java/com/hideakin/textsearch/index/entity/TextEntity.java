package com.hideakin.textsearch.index.entity;

import java.util.Set;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.Table;

import com.hideakin.textsearch.index.model.TextDistribution;

@Entity(name = "texts")
@Table(name = "texts")
@IdClass(TextId.class)
public class TextEntity {

	@Id
	@Column(name="txt")
	private String text;
	
	@Id
	@Column(name="gid")
	private int gid;

	@Column(name="dst")
	private byte[] dist;

	public TextEntity() {
		this(null, -1, null);
	}

	public TextEntity(String text, int gid, byte[] dist) {
		this.text = text;
		this.gid = gid;
		this.dist = dist;
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

	public byte[] getDist() {
		return dist;
	}

	public void setDist(byte[] dist) {
		this.dist = dist;
	}
	
	public boolean hasDist() {
		return dist != null && dist.length > 0;
	}
	
	public void appendDist(TextDistribution.Packed packed) {
		dist = TextDistribution.sequence(dist).append(packed).array();
	}

	public void removeDist(Set<Integer> fids) {
		dist = TextDistribution.sequence(dist).remove(fids).array();
	}

}
