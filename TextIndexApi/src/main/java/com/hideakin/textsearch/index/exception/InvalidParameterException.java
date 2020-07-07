package com.hideakin.textsearch.index.exception;

public class InvalidParameterException extends RuntimeException {

	private static final long serialVersionUID = 4L;

	private String error;

	public InvalidParameterException(String error, String errorDescription) {
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
