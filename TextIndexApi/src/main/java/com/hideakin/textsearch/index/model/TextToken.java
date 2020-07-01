package com.hideakin.textsearch.index.model;

public class TextToken {

	private String text;
	
	private int row;
	
	private int col;
	
	public TextToken(String text, int row, int col) {
		this.text = text;
		this.row = row;
		this.col = col;
	}

	public String getText() {
		return text;
	}

	public int getRow() {
		return row;
	}

	public int getCol() {
		return col;
	}

}
