package com.hideakin.textsearch.index.exception;

public class UnauthorizedException extends CustomRuntimeException {

	private static final long serialVersionUID = 986336393413990532L;

	public UnauthorizedException(String error, String errorDescription) {
		super(error, errorDescription);
	}

}
