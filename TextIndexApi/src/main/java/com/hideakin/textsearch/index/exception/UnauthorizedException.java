package com.hideakin.textsearch.index.exception;

public class UnauthorizedException extends RuntimeException {

	private static final long serialVersionUID = 2L;

	private String error;
	
	public UnauthorizedException(String error, String errorDescription) {
		super(errorDescription);
		this.error = error;
	}

	public String getError() {
		return error;
	}

	public String getErrorDescription() {
		return super.getMessage();
	}

}
