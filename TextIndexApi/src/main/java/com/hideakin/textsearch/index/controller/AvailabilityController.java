package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.service.PreferenceService;

@RestController
public class AvailabilityController {

	@Autowired
	PreferenceService preferenceService;

	@RequestMapping(value="/v1/health/status",method=RequestMethod.GET)
	public String getStatus() {
    	return "OK";
	}

}
