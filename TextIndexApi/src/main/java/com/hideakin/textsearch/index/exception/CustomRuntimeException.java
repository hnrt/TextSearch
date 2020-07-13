package com.hideakin.textsearch.index.exception;

public abstract class CustomRuntimeException extends RuntimeException {

	private static final long serialVersionUID = 1292551008535522560L;

	protected String error;
	
	public CustomRuntimeException(String error, String errorDescription) {
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
