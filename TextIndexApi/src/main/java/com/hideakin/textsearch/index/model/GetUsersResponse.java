package com.hideakin.textsearch.index.model;

public class GetUsersResponse {

	private UserInfo[] values;

	public GetUsersResponse(UserInfo[] users) {
		this.values = users;
	}

	public UserInfo[] getValues() {
		return values;
	}

	public void setValues(UserInfo[] values) {
		this.values = values;
	}

}
