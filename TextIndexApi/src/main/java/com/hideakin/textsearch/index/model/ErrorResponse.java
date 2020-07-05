package com.hideakin.textsearch.index.model;

import com.fasterxml.jackson.annotation.JsonProperty;

public class ErrorResponse {

	@JsonProperty("error")
	private String error;

	@JsonProperty("error_description")
	private String errorDescription;
	
	@JsonProperty("error_uri")
	private String errorUri;

	public ErrorResponse() {
		this(null, null, null);
	}

	public ErrorResponse(String error, String errorDescription) {
		this(error, errorDescription, null);
	}

	public ErrorResponse(String error, String errorDescription, String errorUri) {
		this.error = error;
		this.errorDescription = errorDescription;
		this.errorUri = errorUri;
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

	public String getErrorUri() {
		return errorUri;
	}

	public void setErrorUri(String errorUri) {
		this.errorUri = errorUri;
	}

}
