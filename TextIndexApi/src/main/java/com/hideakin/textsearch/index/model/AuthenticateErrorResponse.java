package com.hideakin.textsearch.index.model;

import com.hideakin.textsearch.index.data.AuthenticateError;

public class AuthenticateErrorResponse extends ErrorResponse {

	public AuthenticateErrorResponse() {
		super(AuthenticateError.INVALID_REQUEST, AuthenticateError.INVALID_REQUEST_DESC);
	}

	public AuthenticateErrorResponse(String error, String errorDescription) {
		super(error, errorDescription);
	}

	public AuthenticateErrorResponse(String error, String errorDescription, String errorUri) {
		super(error, errorDescription, errorUri);
	}

}
