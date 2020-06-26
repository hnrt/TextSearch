package com.hideakin.textsearch.index.exception;

public class UnauthorizedException extends RuntimeException {

	private static final long serialVersionUID = 2L;

	private String error;
	
	private String errorDescription;
	
	public UnauthorizedException(String error, String errorDescription) {
		super("Authorization failed.");
		this.error = error;
		this.errorDescription = errorDescription;
	}

	public String getError() {
		return error;
	}

	public void setError(String error) {
		this.error = error;
	}

	public String getErrorDescription() {
		return errorDescription;
	}

	public void setErrorDescription(String errorDescription) {
		this.errorDescription = errorDescription;
	}

}
