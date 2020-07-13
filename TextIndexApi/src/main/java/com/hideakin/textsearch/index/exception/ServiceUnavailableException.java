package com.hideakin.textsearch.index.exception;

public class ServiceUnavailableException extends CustomRuntimeException {

	private static final long serialVersionUID = 8241428220621362584L;

	public ServiceUnavailableException() {
		super("under_maintenance", "Now under maintenance.");
	}

}
