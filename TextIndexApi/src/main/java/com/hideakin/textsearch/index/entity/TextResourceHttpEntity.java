package com.hideakin.textsearch.index.entity;

import org.springframework.core.io.ByteArrayResource;
import org.springframework.core.io.Resource;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;

public class TextResourceHttpEntity extends HttpEntity<Resource> {

	public TextResourceHttpEntity(byte[] data, String path) {
		super(customResource(data, path), customHeaders());
	}

	private static Resource customResource(byte[] data, String path) {
		return new ByteArrayResource(data) {
			@Override
			public String getFilename() {
                return path;
            }
		};
	}

	private static HttpHeaders customHeaders() {
		HttpHeaders headers = new HttpHeaders();
		headers.add("Content-Type", "text/plain; charset=utf-8");
		return headers;
	}

}
