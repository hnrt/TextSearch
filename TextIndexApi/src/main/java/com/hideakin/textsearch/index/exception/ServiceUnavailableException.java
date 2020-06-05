package com.hideakin.textsearch.index.exception;

public class ServiceUnavailableException extends RuntimeException {

	private static final long serialVersionUID = 1L;

	public ServiceUnavailableException() {
		super("Now under maintenance.");
	}

}
