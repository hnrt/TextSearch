package com.hideakin.textsearch.index.exception;

public class InvalidParameterException extends CustomRuntimeException {

	private static final long serialVersionUID = 838010710987448822L;

	public InvalidParameterException(String error, String errorDescription) {
		super(error, errorDescription);
	}

}
