package com.hideakin.textsearch.index.exception;

public class ForbiddenException extends RuntimeException {

	private static final long serialVersionUID = 3L;
	
	private String error;
	
	public ForbiddenException(String error, String errorDescription) {
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
