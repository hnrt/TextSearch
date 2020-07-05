package com.hideakin.textsearch.index.model;

public class ObjectDisposition {

	public static final int UNDETERMINED = 0;
	public static final int CREATED = 1;
	public static final int UPDATED = 2;
	public static final int GROUP_NOT_FOUND = -1;

	private int value;

	public ObjectDisposition() {
		this.setValue(UNDETERMINED);
	}

	public int getValue() {
		return value;
	}

	public void setValue(int value) {
		this.value = value;
	}

	public boolean isCreated() {
		return value == CREATED;
	}

	public boolean isUpdated() {
		return value == UPDATED;
	}

}
