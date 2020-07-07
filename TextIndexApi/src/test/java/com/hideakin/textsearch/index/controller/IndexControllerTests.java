package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.service.IndexService;

@SpringBootTest
public class IndexControllerTests {

	private MockMvc mockMvc;

	@Mock
	private IndexService indexService;

	@InjectMocks
	private IndexController indexController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(indexController).build();
	}

	@Test
	public void findTextByGroup_successful() throws Exception {
		when(indexService.findText("corge", "XYZZY", SearchOptions.Exact)).thenReturn(
				new TextDistribution[] {
						new TextDistribution(7, new int[] { 55 })
				});
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/index/corge")
				.param("text", "XYZZY")
				.param("option", "Exact"))
	    	.andExpect(status().isOk())
	    	.andExpect(jsonPath("$[0].fid").value(7))
	    	.andExpect(jsonPath("$[0].positions[0]").value(55));
	}

}
