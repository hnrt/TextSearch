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

	@RequestMapping(value="/health/status",method=RequestMethod.GET)
	public String getStatus() {
    	return "OK";
	}

	@RequestMapping(value="/maintenance",method=RequestMethod.GET)
	public String getMaintenanceMode() {
		return preferenceService.isServiceUnavailable() ? "true" : "false";
	}

	@RequestMapping(value="/maintenance",method=RequestMethod.POST)
	public String enterMaintenanceMode() {
		preferenceService.setServiceAvailability(false);
		return "OK";
	}

	@RequestMapping(value="/maintenance",method=RequestMethod.DELETE)
	public String leaveMaintenanceMode() {
		preferenceService.setServiceAvailability(true);
		return "OK";
	}

}
