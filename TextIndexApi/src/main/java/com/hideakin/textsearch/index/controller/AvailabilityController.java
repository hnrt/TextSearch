package com.hideakin.textsearch.index.controller;

import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class AvailabilityController {

	@RequestMapping(value="/health/status",method=RequestMethod.GET)
	public String getStatus() {
    	return "OK";
	}

}
