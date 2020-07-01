package com.hideakin.textsearch.index.exception;

public class ForbiddenException extends RuntimeException {

	private static final long serialVersionUID = 3L;
	
	public ForbiddenException() {
		super("Access denied.");
	}
	
	public ForbiddenException(String message) {
		super(message);
	}
	
}
