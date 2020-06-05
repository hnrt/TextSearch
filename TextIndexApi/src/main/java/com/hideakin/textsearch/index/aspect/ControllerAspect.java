package com.hideakin.textsearch.index.aspect;

import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Before;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.hideakin.textsearch.index.exception.ServiceUnavailableException;
import com.hideakin.textsearch.index.service.PreferenceService;

@Component
@Aspect
public class ControllerAspect {

	@Autowired
	PreferenceService preferenceService;

	@Before("within(com.hideakin.textsearch.index.controller.FileController)")
	public void beforeFileController() {
		if (preferenceService.isServiceUnavailable()) {
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.FileGroupController)")
	public void beforeFileGroupController() {
		if (preferenceService.isServiceUnavailable()) {
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.IndexController)")
	public void beforeIndexController() {
		if (preferenceService.isServiceUnavailable()) {
			throw new ServiceUnavailableException();
		}
	}

	@Before("within(com.hideakin.textsearch.index.controller.PreferenceController)")
	public void beforePreferenceController() {
		if (preferenceService.isServiceUnavailable()) {
			throw new ServiceUnavailableException();
		}
	}

}
