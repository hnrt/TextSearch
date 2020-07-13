package com.hideakin.textsearch.index.exception;

public class ForbiddenException extends CustomRuntimeException {

	private static final long serialVersionUID = 1988725504260498691L;

	public ForbiddenException(String error, String errorDescription) {
		super(error, errorDescription);
	}

}
